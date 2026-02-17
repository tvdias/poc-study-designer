using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Services
{
    [TestClass]
    public class ProductTemplateApplyServiceTests
    {
        private XrmFakedContext _context;
        private readonly Mock<ITracingService> _mockTracing;
        private readonly DataverseContext _dataverseContext;
        private readonly ProductTemplateApplyService _productTemplateApplyService;

        public ProductTemplateApplyServiceTests()
        {

            _context = new XrmFakedContext();
            _mockTracing = new Mock<ITracingService>();
            var service = _context.GetOrganizationService();
            _dataverseContext = new DataverseContext(service);
            _productTemplateApplyService = new ProductTemplateApplyService(_dataverseContext, service, _mockTracing.Object);
        }

        #region 1 - Test if IsDefault true/false works
        /*
         ***** Example 1.1: Test if IsDefault true/false works *****
         *
            Config Question with Dep Rule that Includes Question 'ROCK_MUSIC_RELATED'
            Question 'FAMILIARITY' - Include By Default true
            Question 'ROCK_MUSIC_RELATED' - Include By Default true
            Expected result: include Question 'FAMILIARITY' + Question 'ROCK_MUSIC_RELATED'
         */
        [TestMethod]
        public void ApplyTemplate_IncludeByDefaultTrue_Question_Success()
        {
            // Arrange
            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .Build();
            #endregion

            #region Arrange ConfigQuestion - Which music genre do you prefer
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("A - Rock")
                .Build();
            #endregion

            #region Arrange Dependency Rule to ConfigQuestion
            var questionBankMusic = new QuestionBankBuilder()
                .WithName("ROCK_MUSIC_RELATED")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var dependencyRule = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankMusic)
                .WithTriggeringAnswerIfSingle(configAnswerA)
                .Build();

            var dependencyRules = new List<KTR_DependencyRule> { dependencyRule };
            #endregion

            #region Arrange Template Line - Question
            var questionBankFam = new QuestionBankBuilder()
                .WithName("FAMILIARITY")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var templateLineQuestionFam = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankFam)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(1)
                .Build();
            var templateLineQuestionMusic = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankMusic)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(2)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                productTemplate,
                configQuestion, configAnswerA,
                questionBankMusic, dependencyRule,
                questionBankFam, templateLineQuestionFam, templateLineQuestionMusic
            });

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                dependencyRules);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Where(x => x.QuestionId == questionBankMusic.Id) != null);
            Assert.IsTrue(result.First(x => x.QuestionId == questionBankFam.Id) != null);
        }

        /*
         ***** Example 1.2: Test if IsDefault true/false works *****
         *
            No configuration questions
            Module A (with Question C) - Include By Default true 
            Question B - Include By Default false
            Expected result: only include Module A
         */
        [TestMethod]
        public void ApplyTemplate_IncludeByDefaultFalse_NoDepRules_Success()
        {
            // Arrange
            #region Arrange Project Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            #endregion

            #region Arrange Template Line - Module A
            var moduleA = new ModuleBuilder()
                .WithName("Module A")
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
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
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            var templateLineQuestionB = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankB)
                .WithIncludeByDefault(false)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                productTemplate,
                moduleA, templateLineModuleA, questionBankC, moduleAQuestionBankC,
                questionBankB, templateLineQuestionB
            });

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                new List<KTR_DependencyRule> { });

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Where(x => x.QuestionId == questionBankC.Id) != null);
        }

        /*
         ***** Example 1.3: Test if IsDefault true/false works *****
         *
           Configuration question to include question B and exclude module A
           Module A - Include By Default true
           Question B - Include By Default false
           Expected result: include only question B
        */
        [TestMethod]
        public void ApplyTemplate_IncludeByDefaultFalse_WithDepRules_Success()
        {
            // Arrange
            #region Arrange Project Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template 1")
                .Build();
            #endregion

            # region Arrange ConfigQuestion - Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestionMusic)
               .WithName("B - Pop")
               .Build();
            #endregion

            #region Arrange ConfigQuestion - Gender
            var configQuestionGender = new ConfigurationQuestionBuilder()
                .WithName("Which gender you are?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerGenderMale = new ConfigurationAnswerBuilder(configQuestionGender)
                .WithName("Male")
                .Build();
            var configAnswerGenderFemale = new ConfigurationAnswerBuilder(configQuestionGender)
                .WithName("Female")
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include question B
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
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
            var moduleA = new ModuleBuilder()
               .WithName("Module A")
               .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
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

            var dependencyRules = new List<KTR_DependencyRule> { dependencyRuleInclude, dependencyRuleExclude };

            _context.Initialize(new Entity[]
            {
                productTemplate,
                configQuestionMusic, configAnswerA,
                configQuestionGender, configAnswerGenderMale,
                questionBankB, dependencyRuleInclude, dependencyRuleAnswerA,
                moduleA, questionBankC, dependencyRuleExclude, dependencyRuleAnswerMale,
                templateLineModuleA, templateLineQuestionB
            });

            //Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                dependencyRules);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Where(x => x.QuestionId == questionBankB.Id) != null);
        }
        #endregion

        #region 2 - Test if Multi-Coded implementation works
        /*
         ***** Example 2.1: Test if Multi-Coded implementation works *****
         *
            Multi Configuration question to include question A and question B
            Config Answers: Pop, Rock
            Dep Rules:
            * IF Pop, Include Question A
            * IF Rock, Include Question B
            User answered: Pop + Rock
            Expected result: include question A + B
        */
        [TestMethod]
        public void ApplyTemplate_FilterExactMatchMultiChoiceDependencyRules_DependencyRulesAppliedSuccess()
        {
            // Arrange
            #region Arrange ConfigQuestion - Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerRock = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var configAnswerPop = new ConfigurationAnswerBuilder(configQuestionMusic)
               .WithName("B - Pop")
               .Build();
            #endregion

            #region Arrange QuestionBank - Question A + Question B
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include Question A
            var dependencyRuleA = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankA)
                .Build();
            var dependencyRuleAnswerPop = new DependencyRuleAnswerBuilder(dependencyRuleA, configAnswerPop)
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include question B
            var dependencyRuleB = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .Build();
            var dependencyRuleAnswerRock = new DependencyRuleAnswerBuilder(dependencyRuleB, configAnswerRock)
                .Build();
            #endregion

            var dependencyRules = new List<KTR_DependencyRule> { dependencyRuleB, dependencyRuleA };
            var answerIds = new List<Guid> { configAnswerPop.Id, configAnswerRock.Id };

            // Act
            var result = _productTemplateApplyService.FilterExactMatchMultiChoiceDependencyRules(
                dependencyRules,
                answerIds);

            // Assert
            Assert.AreEqual(result.Count(), dependencyRules.Count());
        }

        /*
         ***** Example 2.2: Test if Multi-Coded implementation works *****
         *
            Multi Configuration question to include question A and question B
            Config Answers: Pop, Rock, Jazz
            Dep Rules:
            * IF Pop AND Rock, Include Question A
            * IF Jazz, Include Question B
            User answered: Rock
            Expected result: empty
        */
        [TestMethod]
        public void ApplyTemplate_FilterExactMatchMultiChoiceDependencyRules_NoDependencyRulesApplied()
        {
            // Arrange
            #region Arrange ConfigQuestion - Music
            var configQuestionMusic = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerRock = new ConfigurationAnswerBuilder(configQuestionMusic)
                .WithName("A - Rock")
                .Build();
            var configAnswerPop = new ConfigurationAnswerBuilder(configQuestionMusic)
               .WithName("B - Pop")
               .Build();
            var configAnswerJazz = new ConfigurationAnswerBuilder(configQuestionMusic)
               .WithName("C - Jazz")
               .Build();
            #endregion

            #region Arrange QuestionBank - Question A + Question B
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include Question A
            var dependencyRuleA = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankA)
                .Build();
            var dependencyRuleAnswerPop = new DependencyRuleAnswerBuilder(dependencyRuleA, configAnswerPop)
                .Build();
            var dependencyRuleAnswerRock = new DependencyRuleAnswerBuilder(dependencyRuleA, configAnswerRock)
                .Build();
            #endregion

            #region Arrange Dependency Rule - Music - Include question B
            var dependencyRuleB = new DependencyRuleBuilder(configQuestionMusic)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankB)
                .Build();
            var dependencyRuleAnswerJazz = new DependencyRuleAnswerBuilder(dependencyRuleB, configAnswerJazz)
                .Build();
            #endregion

            var expectedDependencyRulesToApply = new List<KTR_DependencyRule> { dependencyRuleB, dependencyRuleA };
            var answerIds = new List<Guid> { configAnswerRock.Id };

            // Act
            var result = _productTemplateApplyService.FilterExactMatchMultiChoiceDependencyRules(
                expectedDependencyRulesToApply,
                answerIds);

            // Assert
            Assert.AreEqual(result.Count(), expectedDependencyRulesToApply.Count());
        }
        #endregion

        #region 3 - Test if Order is maintained
        /*
        ***** Example 3.1: Test if Order is maintained *****
        *
           TemplateLines: include 1 - question A, 2 - Module Test (question D + E) and 3 - question C
           Dep Rule:
           * No Dep Rules
           Expected result: 1 - A, 2 - D, 3 - E, 4 - C
       */
        [TestMethod]
        public void ApplyTemplate_OrderedTemplateLines_NoDepRules_OrderIsMaintained()
        {
            // Arrange
            #region Arrange QuestionBank - Question A, B, C, D
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
                .WithQuestionType(KT_QuestionType.MultipleChoice)
                .Build();
            var questionBankD = new QuestionBankBuilder()
                .WithName("Question D")
                .WithQuestionType(KT_QuestionType.SingleChoice)
                .Build();
            var questionBankE = new QuestionBankBuilder()
                .WithName("Question E")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            #endregion

            #region Arrange Module Test
            var moduleTest = new ModuleBuilder()
                .WithName("Module Test")
                .Build();
            var moduleQuestionD = new ModuleQuestionBankBuilder(moduleTest, questionBankD)
                .WithSortOrder(1)
                .Build();
            var moduleQuestionE = new ModuleQuestionBankBuilder(moduleTest, questionBankE)
                .WithSortOrder(2)
                .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template Test")
                .Build();
            #endregion

            #region Arrange Product Template Lines
            var productTemplateLine1 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(1)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankA)
                .Build();
            var productTemplateLine2 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(2)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(moduleTest)
                .Build();
            var productTemplateLine3 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(3)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankC)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                questionBankA, questionBankB, questionBankC, questionBankD,
                moduleTest, moduleQuestionE, moduleQuestionD,
                productTemplate,
                productTemplateLine1, productTemplateLine2, productTemplateLine3
            });

            var expectedQuestionOrder = new Dictionary<int, Guid>
            {
                { 1, questionBankA.Id },
                { 2, questionBankD.Id },
                { 3, questionBankE.Id },
                { 4, questionBankC.Id },
            };

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                new List<KTR_DependencyRule> { });

            // Assert
            Assert.IsNotNull(result);
            foreach (var item in result)
            {
                var expectedOrder = expectedQuestionOrder.First(x => x.Value == item.QuestionId);
                Assert.AreEqual(item.DisplayOrder, expectedOrder.Key);
            }
        }

        /*
        ***** Example 3.2: Test if Order is maintained *****
        *
           TemplateLines: include 1 - question A, 2 - Module Test (question D + E) and 3 - question C
           Config Question: Which gender are you?
           Config Answers: Male, Female
           Dep Rule:
           * IF Female, then EXCLUDE Question D
           Expected result: 1 - A, 2 - E, 3 - C
       */
        [TestMethod]
        public void ApplyTemplate_OrderedTemplateLines_WithDepRules_OrderIsMaintained()
        {
            // Arrange
            #region Arrange QuestionBank - Question A, B, C, D
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
                .WithQuestionType(KT_QuestionType.MultipleChoice)
                .Build();
            var questionBankD = new QuestionBankBuilder()
                .WithName("Question D")
                .WithQuestionType(KT_QuestionType.SingleChoice)
                .Build();
            var questionBankE = new QuestionBankBuilder()
                .WithName("Question E")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            #endregion

            #region Arrange Module Test
            var moduleTest = new ModuleBuilder()
                .WithName("Module Test")
                .Build();
            var moduleQuestionD = new ModuleQuestionBankBuilder(moduleTest, questionBankD)
                .WithSortOrder(1)
                .Build();
            var moduleQuestionE = new ModuleQuestionBankBuilder(moduleTest, questionBankE)
                .WithSortOrder(2)
                .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template Test")
                .Build();
            #endregion

            #region Arrange Product Template Lines
            var productTemplateLine1 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(1)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankA)
                .Build();
            var productTemplateLine2 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(2)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(moduleTest)
                .Build();
            var productTemplateLine3 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(3)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankC)
                .Build();
            #endregion

            #region Arrange Dependency Rule
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which gender are you?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerMale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Male")
                .Build();
            var configAnswerFemale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Female")
                .Build();
            var dependencyRule = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Exclude)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithQuestionBank(questionBankD)
                .WithTriggeringAnswerIfSingle(configAnswerFemale)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                questionBankA, questionBankB, questionBankC, questionBankD,
                moduleTest, moduleQuestionE, moduleQuestionD,
                productTemplate,
                productTemplateLine1, productTemplateLine2, productTemplateLine3,
                configQuestion, configAnswerMale, configAnswerFemale, dependencyRule
            });

            var expectedQuestionOrder = new Dictionary<int, Guid>
            {
                { 1, questionBankA.Id },
                { 2, questionBankE.Id },
                { 3, questionBankC.Id },
            };

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                new List<KTR_DependencyRule> { dependencyRule });

            // Assert
            Assert.IsNotNull(result);
            foreach (var item in result)
            {
                var expectedOrder = expectedQuestionOrder.First(x => x.Value == item.QuestionId);
                Assert.AreEqual(item.DisplayOrder, expectedOrder.Key);
            }
        }

        /*
        ***** Example 3.3: Test if Order is maintained *****
        *
           TemplateLines: include 1 - question A, 2 - Module Test (question D + E) and 3 - question C
           Config Question: Which gender are you?
           Config Answers: Male, Female
           Dep Rule:
           * IF Female, then INCLUDE Question D
           Expected result: 1 - A, 2 - D, 3 - E, 4 - C
       */
        [TestMethod]
        public void ApplyTemplate_OrderedTemplateLines_WithDepRules_IncludeModule_OrderIsMaintained()
        {
            // Arrange
            #region Arrange QuestionBank - Question A, B, C, D
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
                .WithQuestionType(KT_QuestionType.MultipleChoice)
                .Build();
            var questionBankD = new QuestionBankBuilder()
                .WithName("Question D")
                .WithQuestionType(KT_QuestionType.SingleChoice)
                .Build();
            var questionBankE = new QuestionBankBuilder()
                .WithName("Question E")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            #endregion

            #region Arrange Module Test
            var moduleTest = new ModuleBuilder()
                .WithName("Module Test")
                .Build();
            var moduleQuestionE = new ModuleQuestionBankBuilder(moduleTest, questionBankE)
                .WithSortOrder(2)
                .Build();
            var moduleQuestionD = new ModuleQuestionBankBuilder(moduleTest, questionBankD)
                .WithSortOrder(1)
                .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template Test")
                .Build();
            #endregion

            #region Arrange Product Template Lines
            var productTemplateLine1 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(1)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankA)
                .Build();
            var productTemplateLine2 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(false)
                .WithDisplayOrder(2)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(moduleTest)
                .Build();
            var productTemplateLine3 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(3)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankC)
                .Build();
            #endregion

            #region Arrange Dependency Rule
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which gender are you?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerMale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Male")
                .Build();
            var configAnswerFemale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Female")
                .Build();
            var dependencyRule = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Module)
                .WithModule(moduleTest)
                .WithTriggeringAnswerIfSingle(configAnswerFemale)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Secondary)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                questionBankA, questionBankB, questionBankC, questionBankD,
                moduleTest, moduleQuestionE, moduleQuestionD,
                productTemplate,
                productTemplateLine1, productTemplateLine2, productTemplateLine3,
                configQuestion, configAnswerMale, configAnswerFemale, dependencyRule
            });

            var expectedQuestionOrder = new Dictionary<int, Guid>
            {
                { 1, questionBankA.Id },
                { 2, questionBankD.Id },
                { 3, questionBankE.Id },
                { 4, questionBankC.Id },
            };

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                new List<KTR_DependencyRule> { dependencyRule });

            // Assert
            Assert.IsNotNull(result);
            foreach (var item in result)
            {
                var expectedOrder = expectedQuestionOrder.First(x => x.Value == item.QuestionId);
                Assert.AreEqual(item.DisplayOrder, expectedOrder.Key);
            }
        }
        #endregion

        #region 4 - Test if DependencyRules works
        /*
        ***** Example 4.1: Test if DependencyRules works - Tags *****
        *
            TemplateLines: include 1 - question A, 2 - question B (with Tag 1), 3 - question C (with Tag 1)
            Config Question: Which gender are you?
            Config Answers: Male, Female
            Dep Rule:
            * IF Female, then INCLUDE Question B
            Expected result: 1 - B, 2 - C
        */
        [TestMethod]
        public void ApplyTemplate_DepRuleIncludeWithTags_AppliesSuccess()
        {
            // Arrange
            #region Arrange QuestionBank - Question A, B, C, D
            var questionBankA = new QuestionBankBuilder()
                .WithName("Question A")
                .WithQuestionType(KT_QuestionType.MultipleChoiceMatrix)
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .WithName("Question B")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .Build();
            var questionBankC = new QuestionBankBuilder()
                .WithName("Question C")
                .WithQuestionType(KT_QuestionType.MultipleChoice)
                .Build();
            #endregion

            #region Arrange Tag
            var tag = new TagBuilder()
                .WithName("Tag 1")
                .Build();
            var questionTagB = new TagQuestionBankBuilder()
                .WithTagAndQuestionBank(tag, questionBankB)
                .Build();
            var questionTagC = new TagQuestionBankBuilder()
                .WithTagAndQuestionBank(tag, questionBankC)
                .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                .WithName("Template Test")
                .Build();
            #endregion

            #region Arrange Product Template Line
            var productTemplateLine1 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(false)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankA)
                .Build();
            var productTemplateLine2 = new ProductTemplateLineBuilder(productTemplate)
                .WithIncludeByDefault(false)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankB)
                .Build();
            var productTemplateLine3 = new ProductTemplateLineBuilder(productTemplate)
               .WithIncludeByDefault(false)
               .WithType(KTR_ProductTemplateLineType.Question)
               .WithQuestionBank(questionBankC)
               .Build();
            #endregion

            #region Arrange Dependency Rule
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which gender are you?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerMale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Male")
                .Build();
            var configAnswerFemale = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Female")
                .Build();
            var dependencyRule = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Tag)
                .WithTag(tag)
                .WithTriggeringAnswerIfSingle(configAnswerFemale)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .Build();
            #endregion

            _context.Initialize(new Entity[]
            {
                questionBankA, questionBankB, questionBankC,
                tag, questionTagB, questionTagC,
                productTemplate,
                productTemplateLine1, productTemplateLine2, productTemplateLine3,
                configQuestion, configAnswerMale, configAnswerFemale, dependencyRule
            });

            // Act
            var result = _productTemplateApplyService.ApplyProductTemplate(
                productTemplate.Id,
                new List<KTR_DependencyRule> { dependencyRule });

            var expectedQuestions = new List<Guid>()
            {
                { questionBankB.Id },
                { questionBankC.Id }
            };

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, expectedQuestions.Count);
            foreach (var r in result)
            {
                Assert.IsTrue(expectedQuestions.Contains(r.QuestionId));
            }
        }
        #endregion
    }
}
