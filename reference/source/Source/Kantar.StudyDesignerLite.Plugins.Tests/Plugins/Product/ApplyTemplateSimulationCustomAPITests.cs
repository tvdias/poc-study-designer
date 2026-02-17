using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Product;
using Kantar.StudyDesignerLite.Plugins.Project;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product.ApplyTemplateSimulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Product
{
    [TestClass]
    public class ApplyTemplateSimulationCustomAPITests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private readonly string _customAPIName = "ktr_apply_template_simulation_unbound";

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_Success()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .WithProduct(product)
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
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();
            #endregion

            #region Arrange QuestionBank
            var questionBankMusic = new QuestionBankBuilder()
               .WithName("ROCK_MUSIC_RELATED")
               .WithQuestionType(KT_QuestionType.NumericInput)
               .WithTitle("Music related")
               .WithSingleOrMulticoded(KT_SingleOrMultiCode.Singlecode)
               .WithQuestionText("How much do you like rock music?")
               .Build();
            var questionBankMusicAnswer = new QuestionAnswerListBuilder(questionBankMusic)
                .WithText("Music Answer 1")
                .Build();

            var questionBankFam = new QuestionBankBuilder()
                .WithName("FAMILIARITY")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .WithTitle("Familiarity")
                .WithSingleOrMulticoded(KT_SingleOrMultiCode.Multicode)
                .WithQuestionText("How familiar are you with the following topics?")
                .WithAnswerMin(11)
                .WithAnswerMax(21)
                .WithQuestionFormatDetails("KTR_QuestionFormatDetails")
                .WithCustomNotes("KTR_CustomNotes")
                .WithQuestionRationale("KT_QuestionRationale")
                .Build();
            var questionBankFamAnswer = new QuestionAnswerListBuilder(questionBankFam)
                .WithText("YES")
                .Build();

            var questionBankGender = new QuestionBankBuilder()
                .WithName("GENDER")
                .WithQuestionType(KT_QuestionType.SingleChoice)
                .WithTitle("Gender Question")
                .WithQuestionText("What is your gender?")
                .Build();
            var questionBankGender2 = new QuestionBankBuilder()
                .WithName("GENDER 2")
                .WithQuestionType(KT_QuestionType.SingleChoice)
                .WithTitle("Gender Question 2")
                .WithQuestionText("What is your gender? 2")
                .Build();
            var questionBankGenderAnswer = new QuestionAnswerListBuilder(questionBankGender)
                .WithText("FEMALE")
                .Build();
            #endregion

            #region Arrange Module
            var moduleGender = new ModuleBuilder()
               .WithName("GENDER")
               .Build();
            var moduleQuestionBankLink = new ModuleQuestionBankBuilder(moduleGender, questionBankGender)
                .WithSortOrder(1)
                .Build();
            var moduleQuestionBankLink2 = new ModuleQuestionBankBuilder(moduleGender, questionBankGender2)
                .WithSortOrder(2)
                .Build();
            #endregion

            #region Arrange Dependency Rule to Include QuestionBank Music
            var dependencyRule = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Include)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankMusic)
                .WithTriggeringAnswerIfSingle(configAnswerA)
                .Build();
            #endregion

            #region Arrange Dependency Rule to Exclude QuestionBank Gender 2
            var dependencyRule2 = new DependencyRuleBuilder(configQuestion)
                .WithType(KTR_DependencyRule_KTR_Type.Exclude)
                .WithContentType(KTR_DependencyRule_KTR_ContentType.Question)
                .WithClassification(KTR_DependencyRule_KTR_Classification.Primary)
                .WithQuestionBank(questionBankGender2)
                .WithTriggeringAnswerIfSingle(configAnswerA)
                .Build();
            #endregion

            #region Arrange Template Line - Question Fam + Question Music + Module Gender
            var templateLineQuestionFam = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankFam)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(1)
                .Build();
            var templateLineQuestionGender = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(moduleGender)
                .WithIncludeByDefault(true)
                .WithDisplayOrder(2)
                .Build();
            var templateLineQuestionMusic = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(questionBankMusic)
                .WithIncludeByDefault(false)
                .WithDisplayOrder(3)
                .Build();
            #endregion

            var request = BuildRequest(
                product,
                productTemplate,
                configQuestion,
                new List<KTR_ConfigurationAnswer> { configAnswerA });

            var entities = new List<Entity>
            {
                product, productTemplate,
                configQuestion, configAnswerA, productConfigQuestion,
                questionBankFam, questionBankFamAnswer, templateLineQuestionFam,
                questionBankMusic, questionBankMusicAnswer, templateLineQuestionMusic,
                moduleGender, questionBankGender, questionBankGender2, questionBankGenderAnswer, templateLineQuestionGender,
                moduleQuestionBankLink, moduleQuestionBankLink2,
                dependencyRule, dependencyRule2
            };
            var pluginContext = MockPluginContext(
                request,
                entities);

            var expectedOrderedQuestions = new Dictionary<int, KT_QuestionBank>
            {
                { 1, questionBankFam },
                { 2, questionBankGender },
                { 3, questionBankMusic },
            };
            var expectedAnswers = new Dictionary<Guid, KTR_QuestionAnswerList>
            {
                { questionBankFam.Id, questionBankFamAnswer },
                { questionBankMusic.Id, questionBankMusicAnswer },
                { questionBankGender.Id, questionBankGenderAnswer }
            };

            // Act
            _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_response"));
            var response = pluginContext.OutputParameters["ktr_response"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response));

            var result = JsonConvert.DeserializeObject<ApplyTemplateSimulationResponse>(response);
            Assert.AreEqual(result.Questions.Count, expectedOrderedQuestions.Count);

            foreach (var q in result.Questions)
            {
                var expectedOrderQuestion = expectedOrderedQuestions
                    .First(x => x.Value.Id == q.Id);
                Assert.AreEqual(q.DisplayOrder, expectedOrderQuestion.Key);

                Assert.IsTrue(q.Id == expectedOrderQuestion.Value.Id);
                Assert.IsTrue(q.QuestionTitle == expectedOrderQuestion.Value.KT_QuestionTitle);
                Assert.IsTrue(q.QuestionType == expectedOrderQuestion.Value.KT_QuestionType);
                Assert.IsTrue(q.SingleOrMultiCoded == expectedOrderQuestion.Value.KT_SingleOrMultiCode);
                Assert.IsTrue(q.QuestionText == expectedOrderQuestion.Value.KT_DefaultQuestionText);

                Assert.IsTrue(q.Answers != null);
                var expectedQuestionAnswers = expectedAnswers
                    .Where(x => x.Value.KTR_KT_QuestionBank.Id == q.Id);
                Assert.AreEqual(q.Answers.Count, expectedQuestionAnswers.Count());
                foreach (var a in q.Answers)
                {
                    var expectedAnswer = expectedAnswers
                        .First(x => x.Value.Id == a.Id);
                    Assert.IsTrue(a.Id == expectedAnswer.Value.Id);
                    Assert.IsTrue(a.Text == expectedAnswer.Value.KTR_AnswerText);
                }

                if (expectedOrderQuestion.Value.Id == questionBankGender.Id
                    || expectedOrderQuestion.Value.Id == questionBankGender2.Id)
                {
                    Assert.IsTrue(q.Module != null);
                    Assert.IsTrue(q.Module.Id == moduleGender.Id);
                    Assert.IsTrue(q.Module.Name == moduleGender.KT_Name);
                }
                else
                {
                    Assert.IsTrue(q.Module == null);
                }
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_SingleCodedConfigQuestionShouldHaveOnlyOneAnswer_ThrowsError()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .WithProduct(product)
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
            var configAnswerB = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("B - Pop")
                .Build();
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();
            #endregion

            var request = BuildRequest(
                 product,
                 productTemplate,
                 configQuestion,
                 new List<KTR_ConfigurationAnswer> { configAnswerA, configAnswerB });

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    product, productTemplate,
                    configQuestion, configAnswerA, configAnswerB, productConfigQuestion
                });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual($"Single-coded Configuration Questions should have only one answer responded: {configQuestion.Id}", ex.Message);
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_ConfigAnswerSHouldExistInConfigQuestion_ThrowsError()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .WithProduct(product)
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
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();
            #endregion

            var request = BuildRequest(
                 product,
                 productTemplate,
                 configQuestion,
                 new List<KTR_ConfigurationAnswer> { });

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    product, productTemplate,
                    configQuestion, configAnswerA, productConfigQuestion
                });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual($"Configuration Answers not found in Configuration Question: {configQuestion.Id}", ex.Message);
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_ProductShouldExist_ThrowsError()
        {
            // Arrange
            var request = new ApplyTemplateSimulationRequest
            {
                ProductId = Guid.NewGuid(),
                ProductTemplateId = Guid.NewGuid(),
                ConfigurationQuestions = new List<ConfigurationQuestionRequest> { },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity> { });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual("ProductId not found.", ex.Message);
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_ProductTemplateShouldExist_ThrowsError()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            var request = new ApplyTemplateSimulationRequest
            {
                ProductId = product.Id,
                ProductTemplateId = Guid.NewGuid(),
                ConfigurationQuestions = new List<ConfigurationQuestionRequest> { },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity> { product });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual("ProductTemplateId not found.", ex.Message);
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_QuestionShouldExist_ThrowsError()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .WithProduct(product)
                 .Build();
            #endregion

            #region Arrange ConfigQuestion - Which music genre do you prefer
            var configQuestionReq = new ConfigurationQuestionBuilder().Build();
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("A - Rock")
                .Build();
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();
            #endregion

            var request = BuildRequest(
                 product,
                 productTemplate,
                 configQuestionReq,
                 new List<KTR_ConfigurationAnswer> { configAnswerA });

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    product, productTemplate,
                    configQuestion, configAnswerA, productConfigQuestion
                });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual($"Question not found: {configQuestionReq.Id}", ex.Message);
            }
        }

        [TestMethod]
        public void ApplyTemplateSimulationCustomAPI_AnswerShouldExist_ThrowsError()
        {
            // Arrange
            #region Arrange Product
            var product = new ProductBuilder()
                 .WithName("Product 1")
                 .Build();
            #endregion

            #region Arrange Product Template
            var productTemplate = new ProductTemplateBuilder()
                 .WithName("Template 1")
                 .WithProduct(product)
                 .Build();
            #endregion

            #region Arrange ConfigQuestion - Which music genre do you prefer
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Which music genre do you prefer?")
                .WithRule(KTR_Rule.SingleCoded)
                .Build();
            var configAnswerReq = new ConfigurationAnswerBuilder(configQuestion).Build();
            var configAnswerA = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("A - Rock")
                .Build();
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();
            #endregion

            var request = BuildRequest(
                 product,
                 productTemplate,
                 configQuestion,
                 new List<KTR_ConfigurationAnswer> { configAnswerReq });

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    product, productTemplate,
                    configQuestion, configAnswerA, productConfigQuestion
                });

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<ApplyTemplateSimulationCustomAPI>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual($"Answer not found: {configAnswerReq.Id}", ex.Message);
            }
        }

        private ApplyTemplateSimulationRequest BuildRequest(
            KTR_Product product,
            KTR_ProductTemplate productTemplate,
            KTR_ConfigurationQuestion configQuestion,
            List<KTR_ConfigurationAnswer> configAnswers)
        {
            var answerRequestList = new List<ConfigurationAnswerRequest>();

            foreach (var ca in configAnswers)
            {
                answerRequestList.Add(new ConfigurationAnswerRequest
                {
                    Id = ca.Id,
                });
            }

            var configQuestionRequest = new ConfigurationQuestionRequest
            {
                Id = configQuestion.Id,
                Answers = answerRequestList,
            };

            return new ApplyTemplateSimulationRequest
            {
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                ConfigurationQuestions = new List<ConfigurationQuestionRequest> { configQuestionRequest },
            };
        }

        private XrmFakedPluginExecutionContext MockPluginContext(
            ApplyTemplateSimulationRequest request,
            List<Entity> entities)
        {
            _context.Initialize(entities);

            var configQuestionsJson = JsonHelper.Serialize(request.ConfigurationQuestions);

            return new XrmFakedPluginExecutionContext
            {
                MessageName = _customAPIName,
                InputParameters = new ParameterCollection
                {
                    { "productId", request.ProductId },
                    { "productTemplateId", request.ProductTemplateId },
                    { "configurationQuestions", configQuestionsJson }
                },
                OutputParameters = new ParameterCollection
                {
                    { "ktr_response", null }
                }
            };
        }
    }
}
