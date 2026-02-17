using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web.Services.Description;
using System.Workflow.Activities.Rules;
using System.Xml;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Project;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Newtonsoft.Json;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project
{
    [TestClass]
    public class ProjectTemplateApplyCustomAPITests
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

        /*
         Example 0:
            Config Question with Dep Rule that Includes Question 'ROCK_MUSIC_RELATED'
            Question 'FAMILIARITY' - Include By Default true
            Question 'ROCK_MUSIC_RELATED' - Include By Default true
            Expected result: include Question 'FAMILIARITY' + Question 'ROCK_MUSIC_RELATED'
         */
        [TestMethod]
        public void ApplyTemplate_WithIncludeDependencyRule_WithTemplateLineQuestion_AppliesSuccessfully()
        {
            var expectedCountQuestionsApplied = 2;
            var expectedQuestionsApplied = new[] { "ROCK_MUSIC_RELATED", "FAMILIARITY" };

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            #region Arrange ConfigQuestion Multi
            var configQuestionMulti = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.MultiCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionMulti)
                .WithName("A - Rock")
                .Build();
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestionMulti)
               .WithName("B - Pop")
               .Build();
            var projectProductConfigMulti = new ProjectProductConfigBuilder(configQuestionMulti, project)
                .Build();
            var projectProductConfigQuestionAnswerMultiA = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMulti, configAnswerA)
                .Build();
            var projectProductConfigQuestionAnswerMultiB = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMulti, configAnswerB)
                .Build();
            #endregion

            #region Arrange Dependency Rule to ConfigQuestionMulti
            var questionBankMusic = new QuestionBankBuilder()
                .WithName("ROCK_MUSIC_RELATED")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var dependencyRule = new DependencyRuleBuilder(configQuestionMulti)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankMusic)
                .Build();
            var dependencyRuleAnswerA = new DependencyRuleAnswerBuilder(dependencyRule, configAnswerA)
                .Build();
            var dependencyRuleAnswerB = new DependencyRuleAnswerBuilder(dependencyRule, configAnswerB)
                .Build();
            #endregion

            #region Arrange Template Line - Question
            var questionBankFam = new QuestionBankBuilder()
                .WithName("FAMILIARITY")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var templateLineQuestionFam = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankFam)
                .WithIncludeByDefault(true)
                .Build();
            var templateLineQuestionMusic = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankMusic)
                .WithIncludeByDefault(true)
                .Build();
            #endregion

            var entities = new List<Entity> 
            {   
                productTemplate, project,
                configQuestionMulti, configAnswerA, configAnswerB,
                projectProductConfigMulti,
                projectProductConfigQuestionAnswerMultiA,
                projectProductConfigQuestionAnswerMultiB,
                questionBankFam, templateLineQuestionFam, questionBankMusic, templateLineQuestionMusic,
                dependencyRule, dependencyRuleAnswerA, dependencyRuleAnswerB
            };
            var pluginContext = MockPluginContext(project.Id, entities);
            
            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));

            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedCountQuestionsApplied, result.Count);
            Assert.IsTrue(expectedQuestionsApplied.All(x => result.Any(q => q.QuestionName == x)));
        }

        /*
         Example 1:
            No configuration questions
            Module A (with Question C) - Include By Default true 
            Question B - Include By Default false
            Expected result: only include Module A
         */
        [TestMethod]
        public void ApplyTemplate_NoDependencyRule_WithTemplateLinesQuestionAndModule_AppliesSuccessfully()
        {
            var expectedQuestionsApplied = 1;
            var expectedQuestionApplied = "C";

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            #region Arrange Template Line - Module A
            var moduleA = new EntityBuilders.ModuleBuilder()
                .WithName("A")
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("C")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var moduleAQuestionBankC = new ModuleQuestionBankBuilder(moduleA, questionBankC)
                .Build();
            var templateLineModuleA = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(moduleA)
                .WithIncludeByDefault(true)
                .Build();
            #endregion
            #region Arrange Template Line - Question B
            var questionBankB = new QuestionBankBuilder()
                .WithName("B")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            var templateLineQuestionB = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankB)
                .WithIncludeByDefault(false)
                .Build();
            #endregion

            var entities = new List<Entity>
            {
              productTemplate, project,
              moduleA, templateLineModuleA, questionBankC, moduleAQuestionBankC,
              questionBankB, templateLineQuestionB
            };
            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));

            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedQuestionsApplied, result.Count);
            Assert.IsTrue(result.Any(x => x.QuestionName == expectedQuestionApplied));
        }

        /*
         Example 2:
           Configuration question to include question B and exclude module A
           Module A - Include By Default true
           Question B - Include By Default false
           Expected result: include only question B
        */
        [TestMethod]
        public void ApplyTemplate_WithDependencyRuleIncludeExclude_WithTemplateLinesQuestionAndModule_AppliesSuccessfully()
        {
            var expectedCountQuestionsApplied = 1;
            var expectedQuestionsApplied = new[] { "B" };

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            # region Arrange ConfigQuestion Multi Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.MultiCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var projectProductConfigMulti = new ProjectProductConfigBuilder(configQuestionMusic, project)
                .Build();
            var projectProductConfigQuestionAnswerMultiA = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMusic, configAnswerA)
                .WithIsSelected(true)
                .Build();
            #endregion
            #region Arrange ConfigQuestion Multi Gender
            var configQuestionGender = new ConfigurationQuestionBuilder()
                .WithName("Which gender you are?")
                .WithRule(KTR_Rule.MultiCoded)
                .Build();
            var configAnswerGenderMale = new ConfigurationAnswerBuilder(configQuestionGender)
                .WithName("Male")
                .Build();
            var projectProductConfigGender = new ProjectProductConfigBuilder(configQuestionGender, project)
               .Build();
            var projectProductConfigGenderAnswerMale = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigGender, configQuestionGender, configAnswerGenderMale)
                .WithIsSelected(true)
                .Build();
            #endregion

            # region Arrange Dependency Rule - Music - Include question B
            var questionBankB = new QuestionBankBuilder()
                .WithName("B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankB = new QuestionAnswerListBuilder(questionBankB)
                .Build();
            var dependencyRuleInclude = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .Build();
            var dependencyRuleAnswerA = new DependencyRuleAnswerBuilder(dependencyRuleInclude, configAnswerA)
                .Build();
            #endregion
            #region Arrange Dependency Rule - Gender - Exclude Module A
            var moduleA = new EntityBuilders.ModuleBuilder()
               .WithName("A")
               .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("C")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var dependencyRuleExclude = new DependencyRuleBuilder(configQuestionGender)
                .WithType(KTR_DependencyRule_KTR_Type.Exclude)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Module)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithModule(moduleA)
                .Build();
            var dependencyRuleAnswerMale = new DependencyRuleAnswerBuilder(dependencyRuleExclude, configAnswerGenderMale)
                .Build();
            #endregion

            #region Arrange Template Line - Question B + Module A
            var templateLineQuestionB = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankB)
                .WithIncludeByDefault(false)
                .Build();
            var templateLineModuleA = new ProductTemplateLineBuilder(productTemplate)
                .WithModule(moduleA)
                .WithIncludeByDefault(true)
                .WithType(KTR_ProductTemplateLineType.Module)
                .Build();
            #endregion

            var entities = new List<Entity>
            {
                productTemplate, project,
                configQuestionMusic, configAnswerA, projectProductConfigMulti, projectProductConfigQuestionAnswerMultiA,
                configQuestionGender, configAnswerGenderMale, projectProductConfigGender, projectProductConfigGenderAnswerMale,
                questionBankB, answerBankB, dependencyRuleInclude, dependencyRuleAnswerA,
                moduleA, questionBankC, dependencyRuleExclude, dependencyRuleAnswerMale,
                templateLineModuleA, templateLineQuestionB
            };
            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));

            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedCountQuestionsApplied, result.Count);
            Assert.IsTrue(expectedQuestionsApplied.All(x => result.Any(q => q.QuestionName == x)));
        }

        /*
        Example 3: Test Multi-Coded implementation
          Multi Configuration question to include question A and question B
          Config Answers: Pop, Rock
          Question A - Include By Default false
          Question B - Include By Default false
          Dep Rules:
            * IF Pop, Include Question A
            * IF Rock, Include Question B
          User answered: Pop + Rock
          Expected result: include question A + B
       */
        [TestMethod]
        public void ApplyTemplate_WithMultiCodedQuestion_AppliesSuccessfully()
        {
            var expectedCountQuestionsApplied = 2;
            var expectedQuestionsApplied = new[] { "A", "B" };

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            #region Arrange ConfigQuestion - Multi - Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.MultiCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("B - Pop")
                .Build();
            var projectProductConfigMulti = new ProjectProductConfigBuilder(configQuestionMusic, project)
                .Build();
            var projectProductConfigQuestionAnswerMultiA = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMusic, configAnswerA)
                .WithIsSelected(true)
                .Build();
            var projectProductConfigQuestionAnswerMultiB = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMusic, configAnswerB)
                .WithIsSelected(true)
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include question B
            var questionBankA = new QuestionBankBuilder()
                .WithName("A")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankA = new QuestionAnswerListBuilder(questionBankA)
                .Build();
            var dependencyRuleIncludeA = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankA)
                .Build();
            var dependencyRuleAnswerA = new DependencyRuleAnswerBuilder(dependencyRuleIncludeA, configAnswerA)
                .Build();

            var questionBankB = new QuestionBankBuilder()
                .WithName("B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankB = new QuestionAnswerListBuilder(questionBankB)
                .Build();
            var dependencyRuleIncludeB = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .Build();
            var dependencyRuleAnswerB = new DependencyRuleAnswerBuilder(dependencyRuleIncludeB, configAnswerB)
                .Build();
            #endregion

            #region Arrange Template Line - Question B + Question A
            var templateLineQuestionA = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankA)
                .WithIncludeByDefault(false)
                .Build();
            var templateLineQuestionB = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankB)
                .WithIncludeByDefault(false)
                .Build();
            #endregion

            var entities = new List<Entity>
           {
               productTemplate, project,
               configQuestionMusic, configAnswerA, projectProductConfigMulti,
               projectProductConfigQuestionAnswerMultiA,
               questionBankA, answerBankA, dependencyRuleIncludeA, dependencyRuleAnswerA, templateLineQuestionA,
               projectProductConfigQuestionAnswerMultiB,
               questionBankB, answerBankB, dependencyRuleIncludeB, dependencyRuleAnswerB, templateLineQuestionB
           };
            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));

            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedCountQuestionsApplied, result.Count);
            Assert.IsTrue(expectedQuestionsApplied.All(x => result.Any(q => q.QuestionName == x)));
        }

        /*
        Example 4: Test Multi-Coded implementation
          Multi Configuration question to include question A and question B
          Config Answers: Pop, Rock, Jazz
          Question A - Include By Default false
          Question B - Include By Default false
          Dep Rules:
            * IF Pop AND Rock, Include Question A
            * IF Jazz, Include Question B
          User answered: Rock
          Expected result: empty
       */
        [TestMethod]
        public void ApplyTemplate_WithMultiCodedQuestion_ShouldNotApply()
        {
            var expectedCountQuestionsApplied = 0;

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            #region Arrange ConfigQuestion - Multi - Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.MultiCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("B - Pop")
                .Build();
            var configAnswerC = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("C - Jazz")
                .Build();
            var projectProductConfigMulti = new ProjectProductConfigBuilder(configQuestionMusic, project)
                .Build();
            var projectProductConfigQuestionAnswerMultiA = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfigMulti, configQuestionMusic, configAnswerA)
                .WithIsSelected(true)
                .Build();
            #endregion

            #region Arrange Dependency Rules 
            var questionBankA = new QuestionBankBuilder()
                .WithName("A")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankA = new QuestionAnswerListBuilder(questionBankA)
                .Build();
            var dependencyRuleIncludeA = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankA)
                .Build();
            var dependencyRuleAnswerA = new DependencyRuleAnswerBuilder(dependencyRuleIncludeA, configAnswerA)
                .Build();
            var dependencyRuleAnswerB = new DependencyRuleAnswerBuilder(dependencyRuleIncludeA, configAnswerB)
                .Build();

            var questionBankB = new QuestionBankBuilder()
                .WithName("B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankB = new QuestionAnswerListBuilder(questionBankB)
                .Build();
            var dependencyRuleIncludeB = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .Build();
            var dependencyRuleAnswerC = new DependencyRuleAnswerBuilder(dependencyRuleIncludeB, configAnswerC)
                .Build();
            #endregion

            #region Arrange Template Line - Question B + Question A
            var templateLineQuestionA = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankA)
                .WithIncludeByDefault(false)
                .Build();
            var templateLineQuestionB = new ProductTemplateLineBuilder(productTemplate)
                .WithQuestionBank(questionBankB)
                .WithIncludeByDefault(false)
                .Build();
            #endregion

            var entities = new List<Entity>
           {
               productTemplate, project,
               configQuestionMusic, configAnswerA, projectProductConfigMulti, 
               projectProductConfigQuestionAnswerMultiA, 
               questionBankA, answerBankA, dependencyRuleIncludeA, dependencyRuleAnswerA, templateLineQuestionA,
               configAnswerC,
               questionBankB, answerBankB, dependencyRuleIncludeB, dependencyRuleAnswerB, templateLineQuestionB,
               dependencyRuleAnswerC
           };
            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));

            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedCountQuestionsApplied, result.Count);
        }

        /*
        Example 5: Test Exclude in Dep Rule implementation
          Configuration question: Which Gender?
          Config Answers: Binary, Inclusive
          Product template with lines:
            * GENDER SERIES - module (with question GENDER_INCLUSIVE and GENDER_BINARY) - Include By Default true
          Dep Rules:
            * IF Binary, Exclude Question GENDER_INCLUSIVE
            * IF Inclusive, Exclude Question GENDER_BINARY
          User answered: Inclusive
          Expected result: only question GENDER_INCLUSIVE is Included
       */
        [TestMethod]
        public void ApplyTemplate_WithDepRuleExclude_ShouldExclude()
        {
            var expectedCountQuestionsApplied = 1;
            var expectedQuestionsApplied = new[] { "GENDER_INCLUSIVE" };

            #region Arrange Project with Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .WithProductTemplate(productTemplate)
                .Build();
            #endregion

            #region Arrange ConfigQuestion - Gender
            var configQuestionGender = new ConfigurationQuestionBuilder()
                .WithName("Which gender are you?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionGender)
                .WithName("A - Inclusive")
                .Build();
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestionGender)
                .WithName("B - Binary")
                .Build();
            var projectProductConfig = new ProjectProductConfigBuilder(configQuestionGender, project)
                .Build();
            var projectProductConfigQuestionAnswerA = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestionGender, configAnswerA)
                .WithIsSelected(true)
                .Build();
            var projectProductConfigQuestionAnswerB = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestionGender, configAnswerB)
                .WithIsSelected(false)
                .Build();
            #endregion

            #region Arrange Question Banks
            var questionBankA = new QuestionBankBuilder()
                .WithName("GENDER_INCLUSIVE")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankA = new QuestionAnswerListBuilder(questionBankA)
                .Build();

            var questionBankB = new QuestionBankBuilder()
                .WithName("GENDER_BINARY")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var answerBankB = new QuestionAnswerListBuilder(questionBankB)
                .Build();
            #endregion

            #region Arrange Module
            var module = new EntityBuilders.ModuleBuilder()
                .WithName("GENDER SERIES")
                .Build();

            var moduleQuestionA = new ModuleQuestionBankBuilder(module, questionBankA)
                .Build();
            var moduleQuestionB = new ModuleQuestionBankBuilder(module, questionBankB)
                .Build();
            #endregion

            #region Arrange Dependency Rules 
            var dependencyRuleIncludeA = new DependencyRuleBuilder(configQuestionGender)
                .WithType(KTR_DependencyRule_KTR_Type.Exclude)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .WithTriggeringAnswerIfSingle(configAnswerA)
                .Build();

            var dependencyRuleIncludeB = new DependencyRuleBuilder(configQuestionGender)
                .WithType(KTR_DependencyRule_KTR_Type.Exclude)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankA)
                .WithTriggeringAnswerIfSingle(configAnswerB)
                .Build();
            #endregion

            #region Arrange Template Line - Module Gender (Question A + Question B)
            var templateLineQuestionA = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(module)
                .WithIncludeByDefault(true)
                .Build();
            #endregion

            var entities = new List<Entity>
            {
                productTemplate, project,
                configQuestionGender, configAnswerA, configAnswerB,
                projectProductConfig, projectProductConfigQuestionAnswerA, projectProductConfigQuestionAnswerB,
                questionBankA, answerBankA, 
                questionBankB, answerBankB,
                module, moduleQuestionA, moduleQuestionB,
                dependencyRuleIncludeA, dependencyRuleIncludeB,
                templateLineQuestionA
            };
            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ProjectTemplateApplyCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));
            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response), "ktr_response is null or empty.");

            var result = JsonConvert.DeserializeObject<List<TemplateCustomActionResponse>>(response);
            Assert.IsInstanceOfType(result, typeof(List<TemplateCustomActionResponse>));
            Assert.AreEqual(expectedCountQuestionsApplied, result.Count);
            Assert.IsTrue(expectedQuestionsApplied.All(x => result.Any(q => q.QuestionName == x)));
        }

        private XrmFakedPluginExecutionContext MockPluginContext(Guid projectId, List<Entity> entities)
        {
            _context.Initialize(entities);

            _context.AddExecutionMock<ExecuteMultipleRequest>(req =>
            {
                var response = new ExecuteMultipleResponse
                {
                    ["Responses"] = new ExecuteMultipleResponseItemCollection(),
                    ["IsFaulted"] = false
                };
                return response;
            });

            return new XrmFakedPluginExecutionContext
            {
                MessageName = "ktr_apply_template_unbound",
                InputParameters = new ParameterCollection
                {
                    { "ktr_project_id", projectId.ToString() }
                },
                OutputParameters = new ParameterCollection
                {
                    { "ktr_response", null }
                }
            };
        }
    }

    internal class TemplateCustomActionResponse
    {
        public string QuestionName { get; set; }
        public Guid QuestionId { get; set; }
    }
}
