using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Project;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project
{
    [TestClass]
    public class CreateProjectCustomAPITests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private readonly string _customAPIName = "ktr_create_project_unbound";

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void CreateProjectCustomAPI_Success()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product & ProductTemplate
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            #region Arrange QuestionBank
            var standardQuestionBank = new QuestionBankBuilder()
                .WithName("MUSIC_RELATED")
                .WithQuestionType(KT_QuestionType.NumericInput)
                .WithTitle("Music related")
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Standard)
                .Build();
            var questionBankStandardAnswer = new QuestionAnswerListBuilder(standardQuestionBank)
                .WithText("Music Answer 1")
                .Build();

            var customQuestionBank = new QuestionBankBuilder()
                .WithName("FAMILIARITY")
                .WithQuestionType(KT_QuestionType.SingleChoiceMatrix)
                .WithTitle("Familiarity")
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Custom)
                .Build();
            var questionBankCustomAnswer = new QuestionAnswerListBuilder(customQuestionBank)
                .WithText("YES")
                .Build();
            #endregion

            #region Arrange Module
            var moduleA = new ModuleBuilder()
                .WithName("A")
                .Build();
            var moduleQuestionLink = new ModuleQuestionBankBuilder(moduleA, standardQuestionBank)
                .Build();
            #endregion

            #region Environment Variables
            var envVariableOrgUrl = new EnvironmentVariableValueBuilder()
                .WithSchemaName("ktr_OrgUrl")
                .WithValue("Https://test.com")
                .Build();
            var envVariableAppId = new EnvironmentVariableValueBuilder()
                .WithSchemaName("ktr_AppId")
                .WithValue("1234")
                .Build();
            #endregion

            var standardQuestionRequest = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                Id = standardQuestionBank.Id,
                DisplayOrder = 1,
                Module = new ModuleRequest
                {
                    Id = moduleA.Id,
                },
            };
            var customQuestionRequest = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                Id = customQuestionBank.Id,
                DisplayOrder = 2,
            };
            var newAnswerRequestYes = new AnswerRequest
            {
                Name = "YES",
                Text = "Yes sure",
                Location = KTR_AnswerType.Row.ToString(),
            };
            var newAnswerRequestNo = new AnswerRequest
            {
                Name = "NO",
                Text = "Of course not",
            };
            var newCustomQuestionAnswerRequest = new List<AnswerRequest>
            {
                newAnswerRequestYes,
                newAnswerRequestNo,
            };
            var newCustomQuestionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 3,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "QUESTION_CUSTOM_EXAMPLE",
                Title = "Question Title Example",
                Text = "Question Text Example",
                ScripterNotes = "Scriper Notes Example",
                QuestionRationale = "Rationale Example",
                QuestionType = KT_QuestionType.SingleChoiceMatrix.ToString(),
                Answers = newCustomQuestionAnswerRequest,
            };

            var expectedProjectDescription = "Project for Coca-cola Portugal blabla description";
            var expectedProjectName = "Project 1";
            var expectedProjectQuestions = new List<QuestionRequest>
            {
                standardQuestionRequest,
                customQuestionRequest,
                newCustomQuestionRequest,
            };
            var expectedAnswersCount = 4;
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = expectedProjectDescription,
                ProjectName = expectedProjectName,
                Questions = expectedProjectQuestions,
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client,
                    commissioningMarket,
                    product,
                    productTemplate,
                    standardQuestionBank, questionBankStandardAnswer,
                    customQuestionBank, questionBankCustomAnswer,
                    moduleA, moduleQuestionLink,
                    envVariableOrgUrl, envVariableAppId
                });

            // Act
            _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("projectUrl"));
            var response = pluginContext.OutputParameters["projectUrl"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response));
            Assert.IsTrue(pluginContext.OutputParameters.Contains("projectUrl"));

            Assert.IsTrue(Uri.IsWellFormedUriString(response, UriKind.Absolute), $"The value '{response}' is not a valid URL.");
            Assert.IsTrue(response.Contains("etn=kt_project"), "The URL should include the entity logical name 'kt_project'.");

            // Assert project
            var project = _context.CreateQuery<KT_Project>().First();
            Assert.IsNotNull(project);
            Assert.AreEqual(project.KTR_ClientAccount.Id, client.Id);
            Assert.AreEqual(project.KT_CommissioningMarket.Id, commissioningMarket.Id);
            Assert.AreEqual(project.KTR_Product.Id, product.Id);
            Assert.AreEqual(project.KTR_ProductTemplate.Id, productTemplate.Id);
            Assert.AreEqual(project.KT_Description, expectedProjectDescription);
            Assert.AreEqual(project.KT_Name, expectedProjectName);

            // Assert QuestionnaireLines
            var questionnaireLines = _context.CreateQuery<KT_QuestionnaireLines>().ToList();
            Assert.AreEqual(questionnaireLines.Count, expectedProjectQuestions.Count);
            Assert.IsTrue(questionnaireLines.All(x => x.KTR_Project.Id == project.Id));

            foreach (var ql in questionnaireLines)
            {
                if (ql.KTR_QuestionBank != null)
                {
                    var questionRequest = expectedProjectQuestions
                        .Where(x => x.Origin == QuestionRequestOrigin.QuestionBank)
                        .Select(x => (ExistingQuestionRequest)x)
                        .FirstOrDefault(x => x.Id == ql.KTR_QuestionBank.Id);
                    Assert.AreEqual(ql.KT_QuestionSortOrder, questionRequest.DisplayOrder);
                    Assert.AreEqual(ql.KTR_QuestionBank.Id, questionRequest.Id);
                    if (questionRequest.Module != null)
                    {
                        Assert.AreEqual(ql.KTR_Module.Id, questionRequest.Module.Id);
                    }
                    else
                    {
                        Assert.IsNull(ql.KTR_Module);
                    }

                }
                else
                {
                    var questionRequest = expectedProjectQuestions
                        .Where(x => x.Origin == QuestionRequestOrigin.New)
                        .Select(x => (NewQuestionRequest)x)
                        .FirstOrDefault(x => x.VariableName == ql.KT_QuestionVariableName);
                    Assert.AreEqual(ql.KT_QuestionSortOrder, questionRequest.DisplayOrder);
                    Assert.AreEqual(ql.KT_StandardOrCustom.ToString(), questionRequest.StandardOrCustom);
                    Assert.AreEqual(ql.KT_QuestionVariableName, questionRequest.VariableName);
                    Assert.AreEqual(ql.KT_QuestionTitle, questionRequest.Title);
                    Assert.AreEqual(ql.KT_QuestionText2, questionRequest.Text);
                    Assert.AreEqual(ql.KTR_ScripterNotes, questionRequest.ScripterNotes);
                    Assert.AreEqual(ql.KTR_QuestionRationale, questionRequest.QuestionRationale);
                    Assert.AreEqual(ql.KT_QuestionType.ToString(), questionRequest.QuestionType);
                }
            }

            // Assert QuesitonnaireLineAnswers
            var questionnaireLinesAnswers = _context.CreateQuery<KTR_QuestionnaireLinesAnswerList>().ToList();
            Assert.AreEqual(questionnaireLinesAnswers.Count, expectedAnswersCount);

            foreach (var ql in questionnaireLines)
            {
                var answers = questionnaireLinesAnswers
                    .Where(x => x.KTR_QuestionnaireLine.Id == ql.Id);
                Assert.IsTrue(answers.Count() > 0);

                if (ql.KT_QuestionVariableName == newCustomQuestionRequest.VariableName)
                {
                    foreach (var a in answers)
                    {
                        var answerReq = newCustomQuestionAnswerRequest
                            .FirstOrDefault(x => x.Name == a.KTR_Name);
                        Assert.AreEqual(answerReq.Name, a.KTR_Name);
                        Assert.AreEqual(answerReq.Text, a.KTR_AnswerText);
                    }
                }
                else
                {
                    Assert.AreEqual(1, answers.Count());
                    var a = answers.FirstOrDefault();

                    if (ql.KTR_QuestionBank.Id == standardQuestionBank.Id)
                    {
                        Assert.AreEqual(questionBankStandardAnswer.KTR_Name, a.KTR_Name);
                        Assert.AreEqual(questionBankStandardAnswer.KTR_AnswerText, a.KTR_AnswerText);
                    }
                    else
                    {
                        Assert.AreEqual(questionBankCustomAnswer.KTR_Name, a.KTR_Name);
                        Assert.AreEqual(questionBankCustomAnswer.KTR_AnswerText, a.KTR_AnswerText);
                    }
                }
            }
        }

        [TestMethod]
        public void CreateProjectCustomAPI_InvalidClientId_ThrowsException()
        {
            // Arrange
            var questionRequest = new ExistingQuestionRequest
            {
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = Guid.NewGuid(),
                CommissioningMarketId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductTemplateId = Guid.NewGuid(),
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "ClientId not found.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_InvalidComissioningMarketId_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new ExistingQuestionRequest
            {
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = Guid.NewGuid(),
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "CommissioningMarketId not found.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_EmptyQuestions_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Questions are required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_InvalidQuestions_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Questions not found in Question Bank.",
                exception.Message
            );
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("InvalidSomething")]
        [DataRow("Standard")]
        public void CreateProjectCustomAPI_TryToCreateStandardQuestion_ThrowsException(string standardOrCustom)
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = standardOrCustom,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Only Custom Questions can be created.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_InvalidModule_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            #region Arrange QuestionBank
            var questionBank = new QuestionBankBuilder()
                .Build();
            #endregion

            var questionRequest = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                DisplayOrder = 1,
                Id = questionBank.Id,
                Module = new ModuleRequest
                {
                    Id = Guid.NewGuid(),
                }
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, questionBank, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                $"Module not found in Question: {questionBank.Id}.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_InvalidDisplayOrder_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange QuestionBank
            var questionBankA = new QuestionBankBuilder()
                .Build();
            var questionBankB = new QuestionBankBuilder()
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequestA = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                DisplayOrder = 1,
                Id = questionBankA.Id,
            };
            var questionRequestB = new ExistingQuestionRequest
            {
                Origin = QuestionRequestOrigin.QuestionBank,
                DisplayOrder = 1,
                Id = questionBankB.Id,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequestA, questionRequestB
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, questionBankA, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "DisplayOrder in Questions must be sequential starting from 1 without gaps or duplicates.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_VariableIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "",
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - VariableName is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_TitleIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "",
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - Title is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_TextIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "",
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - Text is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_ScripterNotesIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "",
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - ScripterNotes is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_QuestionRationaleIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "",
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - QuestionRationale is required.",
                exception.Message
            );
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("invalidSomething")]
        public void CreateProjectCustomAPI_CreateCustomQuestion_QuestionTypeIsRequired_ThrowsException(string questionType)
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = questionType,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            if (string.IsNullOrWhiteSpace(questionType))
            {
                Assert.AreEqual(
                    "Error Creating Custom Question - QuestionType is required.",
                    exception.Message
                );
            }
            else
            {
                Assert.AreEqual(
                    "Error Creating Custom Question - QuestionType is invalid.",
                    exception.Message
                );
            }
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_AnswersAreRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = KT_QuestionType.SingleChoiceMatrix.ToString(),
                Answers = new List<AnswerRequest>() { },
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Custom Question - Answers are required for some QuestionTypes.",
                exception.Message
            );
        }

        [DataTestMethod]
        [DataRow(KT_QuestionType.SmallTextInput)]
        [DataRow(KT_QuestionType.DisplayScreen)]
        [DataRow(KT_QuestionType.LargeTextInput)]
        [DataRow(KT_QuestionType.Logic)]
        [DataRow(KT_QuestionType.NumericInput)]
        [DataRow(KT_QuestionType.TextInputMatrix)]
        public void CreateProjectCustomAPI_CreateCustomQuestion_AnswersNotRequired_OpenQuestions_ShouldNotThrowException(
            KT_QuestionType questionType)
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            #region Environment Variables
            var envVariableOrgUrl = new EnvironmentVariableValueBuilder()
                .WithSchemaName("ktr_OrgUrl")
                .WithValue("Https://test.com")
                .Build();
            var envVariableAppId = new EnvironmentVariableValueBuilder()
                .WithSchemaName("ktr_AppId")
                .WithValue("1234")
                .Build();
            #endregion

            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = questionType.ToString(),
                Answers = null,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate,
                    envVariableOrgUrl, envVariableAppId
                });

            // Act
            _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.Contains("projectUrl"));
            var response = pluginContext.OutputParameters["projectUrl"] as string;
            Assert.IsFalse(string.IsNullOrEmpty(response));
            Assert.IsTrue(pluginContext.OutputParameters.Contains("projectUrl"));
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_AnswersNameIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var answerRequest = new AnswerRequest
            {
                Name = "",
            };
            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = KT_QuestionType.SingleChoiceMatrix.ToString(),
                Answers = new List<AnswerRequest>()
                {
                    answerRequest
                },
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Answers in Custom Question - Answer Name is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_AnswersTextIsRequired_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var answerRequest = new AnswerRequest
            {
                Name = "test",
                Text = "",
            };
            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = KT_QuestionType.SingleChoiceMatrix.ToString(),
                Answers = new List<AnswerRequest>()
                {
                    answerRequest
                },
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Answers in Custom Question - Answer Text is required.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_CreateCustomQuestion_AnswersLocationIsInvalid_ThrowsException()
        {
            // Arrange
            #region Arrange Client
            var client = new ClientBuilder()
                .WithName("Coca-cola")
                .Build();
            #endregion

            #region Arrange CommissioningMarket
            var commissioningMarket = new CommissioningMarketBuilder()
                .WithName("Portugal")
                .Build();
            #endregion

            #region Arrange Product
            var product = new ProductBuilder()
                .WithName("ProductA")
                .Build();
            #endregion

            #region Arrange ProductTemplate
            var productTemplate = new ProductTemplateBuilder()
                .WithName("TemplateA")
                .Build();
            #endregion

            var answerRequest = new AnswerRequest
            {
                Name = "test",
                Text = "test",
                Location = "invalid",
            };
            var questionRequest = new NewQuestionRequest
            {
                Origin = QuestionRequestOrigin.New,
                DisplayOrder = 1,
                StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Custom.ToString(),
                VariableName = "test",
                Title = "test",
                Text = "test",
                ScripterNotes = "test",
                QuestionRationale = "test",
                QuestionType = KT_QuestionType.SingleChoiceMatrix.ToString(),
                Answers = new List<AnswerRequest>()
                {
                    answerRequest
                },
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest>
                {
                    questionRequest
                },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity>
                {
                    client, commissioningMarket, product, productTemplate
                });

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
                "Error Creating Answers in Custom Question - Answer Location is invalid.",
                exception.Message
            );
        }

        [TestMethod]
        public void CreateProjectCustomAPI_ProductIdMissing_ProjectCreatedWithoutProduct()
        {
            // Arrange
            var client = new ClientBuilder().WithName("Coca-cola").Build();
            var commissioningMarket = new CommissioningMarketBuilder().WithName("Portugal").Build();
            var productTemplate = new ProductTemplateBuilder().WithName("TemplateA").Build();

            var questionRequest = new ExistingQuestionRequest
            {
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = Guid.Empty, // ProductId missing
                ProductTemplateId = productTemplate.Id,
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest> { questionRequest },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity> { client, commissioningMarket, productTemplate }
            );

            // Act
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            // Assert
            Assert.AreEqual("Only Custom Questions can be created.", exception.Message);
        }

        [TestMethod]
        public void CreateProjectCustomAPI_ProductTemplateIdMissing_ProjectCreatedWithoutProductTemplate()
        {
            // Arrange
            var client = new ClientBuilder().WithName("Coca-cola").Build();
            var commissioningMarket = new CommissioningMarketBuilder().WithName("Portugal").Build();
            var product = new ProductBuilder().WithName("ProductA").Build();

            var questionRequest = new ExistingQuestionRequest
            {
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = product.Id,
                ProductTemplateId = Guid.Empty, // ProductTemplateId missing
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest> { questionRequest },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity> { client, commissioningMarket, product }
            );

            // Act
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            // Assert
            Assert.AreEqual("Only Custom Questions can be created.", exception.Message);
        }

        [TestMethod]
        public void CreateProjectCustomAPI_ProductIdAndProductTemplateIdMissing_ProjectCreatedWithoutBoth()
        {
            // Arrange
            var client = new ClientBuilder().WithName("Coca-cola").Build();
            var commissioningMarket = new CommissioningMarketBuilder().WithName("Portugal").Build();

            var questionRequest = new ExistingQuestionRequest
            {
                Id = Guid.NewGuid(),
                DisplayOrder = 1,
            };
            var request = new CreateProjectRequest
            {
                ClientId = client.Id,
                CommissioningMarketId = commissioningMarket.Id,
                ProductId = Guid.Empty, // ProductId missing
                ProductTemplateId = Guid.Empty, // ProductTemplateId missing
                Description = "test",
                ProjectName = "test",
                Questions = new List<QuestionRequest> { questionRequest },
            };

            var pluginContext = MockPluginContext(
                request,
                new List<Entity> { client, commissioningMarket }
            );

            // Act
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<CreateProjectCustomAPI>(pluginContext);
            });

            // Assert
            Assert.AreEqual("Only Custom Questions can be created.", exception.Message);
        }

        private XrmFakedPluginExecutionContext MockPluginContext(
            CreateProjectRequest request,
            List<Entity> entities)
        {
            _context.Initialize(entities);

            _context.AddExecutionMock<ExecuteMultipleRequest>(raw =>
            {
                var req = (ExecuteMultipleRequest)raw;

                var resp = new ExecuteMultipleResponse
                {
                    ["Responses"] = new ExecuteMultipleResponseItemCollection(),
                    ["IsFaulted"] = false
                };

                var innerService = _context.GetOrganizationService();

                foreach (var inner in req.Requests)
                {
                    innerService.Execute(inner); 
                }

                return resp;
            });

            var questionJson = JsonHelper.Serialize(request.Questions);
            return new XrmFakedPluginExecutionContext
            {
                MessageName = _customAPIName,
                InputParameters = new ParameterCollection
                {
                    { "clientId", request.ClientId },
                    { "commissioningMarketId", request.CommissioningMarketId },
                    { "description", request.Description },
                    { "productId", request.ProductId },
                    { "productTemplateId", request.ProductTemplateId },
                    { "projectName", request.ProjectName },
                    { "questions", questionJson },
                },
                OutputParameters = new ParameterCollection
                {
                    { "projectUrl", null }
                }
            };
        }
    }
}
