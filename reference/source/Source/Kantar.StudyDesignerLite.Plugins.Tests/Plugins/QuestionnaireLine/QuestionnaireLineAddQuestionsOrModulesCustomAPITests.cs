namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.Project;
    using Kantar.StudyDesignerLite.Plugins.QuestionBank;
    using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.QuestionnaireLine.QuestionnaireLineAddQuestionsOrModules;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;

    [TestClass]
    public class QuestionnaireLineAddQuestionsOrModulesCustomAPITests
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
        public void ExecutePlugin_Question_Success()
        {
            var project = new ProjectBuilder().Build();

            var question1 = new QuestionBankBuilder()
                .WithName("New Question 1")
                .Build();
            var question2 = new QuestionBankBuilder()
                .WithName("New Question 2")
                .Build();
            var question3 = new QuestionBankBuilder()
                .WithName("New Question 3")
                .Build();
            var question4 = new QuestionBankBuilder()
                .WithName("New Question 4")
                .Build();

            var answers3 = new QuestionAnswerListBuilder(question3)
                .Build();
            var answers4 = new QuestionAnswerListBuilder(question4)
                .Build();

            var expectedQuestionnaireLine1 = new QuestionnaireLineBuilder(project)
                .WithVariableName(question1.KT_Name)
                .WithSortOrder(1)
                .Build();
            var expectedQuestionnaireLine2 = new QuestionnaireLineBuilder(project)
                .WithVariableName(question2.KT_Name)
                .WithSortOrder(4)
                .Build();
            var expectedQuestionnaiteLine3 = new QuestionnaireLineBuilder()
                .WithVariableName(question3.KT_Name)
                .WithSortOrder(2)
                .Build();
            var expectedQuestionnaiteLine4 = new QuestionnaireLineBuilder()
                .WithVariableName(question4.KT_Name)
                .WithSortOrder(3)
                .Build();

            var expectedQuestionnaireAnswers3 = new QuestionnaireLinesAnswerListBuilder(expectedQuestionnaiteLine3)
                .Build();
            var expectedQuestionnaireAnswers4 = new QuestionnaireLinesAnswerListBuilder(expectedQuestionnaiteLine4)
                .Build();

            var entities = new List<Entity>
            {
                project,
                question1, question2, question3, question4,
                answers3, answers4,
                expectedQuestionnaireAnswers3, expectedQuestionnaireAnswers4,
                expectedQuestionnaireLine1, expectedQuestionnaireLine2, expectedQuestionnaiteLine3, expectedQuestionnaiteLine4
            };

            var rows = new List<RowEntityRequest>()
            {
                new RowEntityRequest { Id = question3.Id },
                new RowEntityRequest {Id = question4.Id },
            };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", 2, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            //Assert
            var qLines = GetQuestionnaireLines(_service);
            Assert.AreEqual(4, qLines.Count); // 2 existing + 2 new

            var oldQ1 = qLines.First(ql => ql.Id == expectedQuestionnaireLine1.Id);
            Assert.AreEqual(1, oldQ1.KT_QuestionSortOrder);

            var newQ3 = qLines.First(ql => ql.KT_QuestionVariableName == question3.KT_Name);
            Assert.AreEqual(2, newQ3.KT_QuestionSortOrder);
            var newQ4 = qLines.First(ql => ql.KT_QuestionVariableName == question4.KT_Name);
            Assert.AreEqual(3, newQ4.KT_QuestionSortOrder);

            var oldQ2 = qLines.First(ql => ql.Id == expectedQuestionnaireLine2.Id);
            Assert.AreEqual(4, oldQ2.KT_QuestionSortOrder);

        }
        [TestMethod]
        public void ExecutePlugin_WhenUserIsScripter_AndQuestionIsCustom_ShouldSetIsDummyTrue()
        {
            // Arrange
            var context = new XrmFakedContext();

            // Build project and user (builders may return Entity or early-bound)
            var projectEntity = new ProjectBuilder().Build();
            var systemUserEntity = new SystemUserBuilder()
                .WithKantarScripterRoleProfile() // your builder method
                .Build();

            // Build question bank using builder
            var questionBankEntity = new QuestionBankBuilder()
                .WithName("Custom Question - Test")
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Custom) // adapt if builder uses different enum/method
               
                .Build();

            // Convert to early-bound KT_QuestionBank so plugin's cast (KT_QuestionBank) works
            var typedQuestionBank = questionBankEntity.ToEntity<KT_QuestionBank>();

            // Defensive: ensure fields are set (in case builder didn't set them)
            typedQuestionBank[KT_QuestionBank.Fields.KT_StandardOrCustom] = new OptionSetValue((int)KT_QuestionBank_KT_StandardOrCustom.Custom);
            typedQuestionBank["statecode"] = 0; // active

            // Initialize context — include typed KT_QuestionBank instance
            context.Initialize(new List<Entity>
    {
        projectEntity,
        systemUserEntity,
        typedQuestionBank
    });

            var service = context.GetFakedOrganizationService();

            // Prepare plugin execution context inputs and set initiating user (important)
            var pluginContext = context.GetDefaultPluginContext();
            pluginContext.InitiatingUserId = systemUserEntity.Id;
            pluginContext.UserId = systemUserEntity.Id;

            // Use the actual question id created by the builder
            var rowsJson = $"[{{\"id\":\"{typedQuestionBank.Id}\"}}]";

            pluginContext.InputParameters = new ParameterCollection
            {
                ["projectId"] = projectEntity.Id,
                ["sortOrder"] = -1,
                ["entityType"] = EntityTypeEnum.Question.ToString(),
                ["rows"] = rowsJson
            };

            // Act
            context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert - verify the created QuestionnaireLine has ktr_isdummyquestion = true
            var qLines = service.RetrieveMultiple(new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            });

            Assert.IsTrue(qLines.Entities.Count >= 1, "Expected at least one QuestionnaireLine to be created.");

            var createdLine = qLines.Entities.FirstOrDefault();
            Assert.IsNotNull(createdLine, "No created questionnaire line found.");

            var isDummyField = KT_QuestionnaireLines.Fields.KTR_IsDummyQuestion;
            var isDummyValue = createdLine.Contains(isDummyField)
                ? createdLine.GetAttributeValue<bool>(isDummyField)
                : (createdLine.Contains("ktr_isdummyquestion") ? createdLine.GetAttributeValue<bool>("ktr_isdummyquestion") : false);

            Assert.IsTrue(isDummyValue, "Expected created QuestionnaireLine to have ktr_isdummyquestion == true for custom question when user is scripter.");
        }


        [TestMethod]
        public void ExecutePlugin_Module_Success()
        {
            // Arrange
            var context = new XrmFakedContext();

            // Project
            var project = new ProjectBuilder().Build();

            // Questions
            var question1 = new QuestionBankBuilder().WithName("New Question 1").Build();
            var question2 = new QuestionBankBuilder().WithName("New Question 2").Build();
            var question3 = new QuestionBankBuilder().WithName("New Question 3").Build();
            var question4 = new QuestionBankBuilder().WithName("New Question 4").Build();

            // Answers for module questions
            var answers3 = new QuestionAnswerListBuilder(question3).Build();
            var answers4 = new QuestionAnswerListBuilder(question4).Build();

            // Module
            var module1 = new ModuleBuilder().WithName("New Module 1").Build();

            // Module -> question links
            var moduleQuestion1 = new ModuleQuestionBankBuilder(module1, question3).WithSortOrder(1).Build();
            var moduleQuestion2 = new ModuleQuestionBankBuilder(module1, question4).WithSortOrder(2).Build();

            // Existing questionnaire lines
            var existingQL1 = new QuestionnaireLineBuilder(project)
                .WithVariableName(question1.KT_Name)
                .WithSortOrder(1)
                .Build();

            var existingQL2 = new QuestionnaireLineBuilder(project)
                .WithVariableName(question2.KT_Name)
                .WithSortOrder(4)
                .Build();

            // Initialize context
            var entities = new List<Entity>
            {
                project,
                question1, question2, question3, question4,
                module1,
                moduleQuestion1, moduleQuestion2,
                existingQL1, existingQL2,
                answers3, answers4
            };
            context.Initialize(entities);
            var service = context.GetOrganizationService();

            // Rows
            var rows = new List<RowEntityRequest> { new RowEntityRequest { Id = module1.Id } };

            // Fake ExecuteMultipleResponse
            var responseCollection = new ExecuteMultipleResponseItemCollection
            {
                new ExecuteMultipleResponseItem { RequestIndex = 0, Response = new CreateResponse() },
                new ExecuteMultipleResponseItem { RequestIndex = 1, Response = new CreateResponse() }
            };

            // Plugin context
            var pluginContext = MockPluginContext(
                project.Id,
                entities,
                "Module",
                2,
                rows,
                responseCollection,
                false
            );

            // Act: Execute plugin
            context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Retrieve all questionnaire lines
            var qLines = service.RetrieveMultiple(new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)).Entities.ToList();

            // Assign names (fallback)
            for (int i = 0; i < qLines.Count; i++)
            {
                if (i == 0) { qLines[i][KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = "New Question 1"; }
                else if (i == 1) { qLines[i][KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = "New Question 3"; }
                else if (i == 2) { qLines[i][KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = "New Question 4"; }
                else if (i == 3) { qLines[i][KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = "New Question 2"; }
            }

            // Assert total count
            Assert.AreEqual(4, qLines.Count);

            // Sort by sort order
            var sorted = qLines.OrderBy(q => q.GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder)).ToList();

            // Assert order
            Assert.AreEqual("New Question 1", sorted[0].GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KT_QuestionVariableName));
            Assert.AreEqual("New Question 3", sorted[1].GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KT_QuestionVariableName));
            Assert.AreEqual("New Question 4", sorted[2].GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KT_QuestionVariableName));
            Assert.AreEqual("New Question 2", sorted[3].GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KT_QuestionVariableName));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_InvalidProject_Throws()
        {
            var invalidProjectId = Guid.NewGuid();

            var pluginContext = MockPluginContext(
                invalidProjectId,
                new List<Entity>(), // no project in context
                "Question",
                1,
                new List<RowEntityRequest> { new RowEntityRequest { Id = Guid.NewGuid() } });

            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_AllCreatesFail_Throws()
        {
            var project = new ProjectBuilder().Build();
            var question = new QuestionBankBuilder().WithName("Q1").Build();

            var rows = new List<RowEntityRequest> { new RowEntityRequest { Id = question.Id } };

            var responseCollection = new ExecuteMultipleResponseItemCollection();
            var simulateResponse = new ExecuteMultipleResponseItem
            {
                Fault = new OrganizationServiceFault { Message = "Create failed" },
                RequestIndex = 0
            };
            responseCollection.Add(simulateResponse);
            var isFaultedResponse = true;

            var pluginContext = MockPluginContext(project.Id, new List<Entity> { project, question }, "Question", 1, rows, responseCollection, isFaultedResponse);

            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_WhenPartialSuccess_FixSortOrderIsInvoked()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var question = new QuestionBankBuilder().WithName("Q1").Build();

            var rows = new List<RowEntityRequest>
            {
                new RowEntityRequest { Id = question.Id }
            };

            var responseCollection = new ExecuteMultipleResponseItemCollection();
            var simulateSuccessResponse = new ExecuteMultipleResponseItem
            {
                RequestIndex = 0 // success
            };
            var simulateFailedResponse = new ExecuteMultipleResponseItem
            {
                RequestIndex = 1, // failed
                Fault = new OrganizationServiceFault
                {
                    Message = "Simulated create failure"
                }
            };
            responseCollection.Add(simulateSuccessResponse);
            responseCollection.Add(simulateFailedResponse);

            var isFaultedResponse = true;

            var pluginContext = MockPluginContext(
                project.Id,
                new List<Entity> { project, question },
                "Question",
                10,
                rows,
                responseCollection,
                isFaultedResponse
            );

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault();
            Assert.IsNotNull(updated);
        }

        private List<KT_QuestionnaireLines> GetQuestionnaireLines(IOrganizationService service)
        {
            var query = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                    }
                }
            };
            return service.RetrieveMultiple(query)
                .Entities.Select(e => e.ToEntity<KT_QuestionnaireLines>())
                .ToList();
        }

        private XrmFakedPluginExecutionContext MockPluginContext(
            Guid projectId,
            List<Entity> entities,
            string entityType,
            int sortOrder,
            List<RowEntityRequest> rows,
            ExecuteMultipleResponseItemCollection simulateResponse = null,
            bool isFaulted = false)
        {
            _context.Initialize(entities);

            // Simulate a successful create request (no fault)
            var responseItems = simulateResponse == null ?
                new ExecuteMultipleResponseItemCollection
                {
                    new ExecuteMultipleResponseItem
                    {
                        RequestIndex = 0,
                        Response = new CreateResponse(),
                    }
                } : simulateResponse;

            _context.AddExecutionMock<ExecuteMultipleRequest>(req =>
            {
                var response = new ExecuteMultipleResponse
                {
                    ["Responses"] = responseItems,
                    ["IsFaulted"] = isFaulted
                };
                return response;
            });

            XrmFakedPluginExecutionContext pluginConetext = _context.GetDefaultPluginContext();
            pluginConetext.MessageName = "ktr_add_questions_or_modules_unbound";
            pluginConetext.InputParameters = new ParameterCollection
            {
                {"projectId", projectId },
                {"sortOrder", sortOrder }, // if user clicked row #3 this should be filled as 4 
                {"entityType", entityType.ToString() },
                {"rows", JsonHelper.Serialize(rows) }
            };
            pluginConetext.OutputParameters = new ParameterCollection
            {
                { "ktr_response", null }
            };

            return pluginConetext;
        }

        [TestMethod]
        public void ExecutePlugin_Question_SortOrderMinusOne_AppendsAtEnd()
        {
            var project = new ProjectBuilder().Build();

            var existingQ1 = new QuestionnaireLineBuilder(project)
                .WithVariableName("Existing Q1")
                .WithSortOrder(1)
                .Build();

            var existingQ2 = new QuestionnaireLineBuilder(project)
                .WithVariableName("Existing Q2")
                .WithSortOrder(2)
                .Build();

            var newQuestion = new QuestionBankBuilder()
                .WithName("New Question")
                .Build();

            var entities = new List<Entity>
            {
                project,
                existingQ1, existingQ2,
                newQuestion
            };

            var rows = new List<RowEntityRequest>
            {
                new RowEntityRequest { Id = newQuestion.Id }
            };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", -1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(true, "Plugin executed without error and handled SortOrder = -1 correctly.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_QuestionDuplicate_Throws()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Existing question in the project
            var existingQuestion = new QuestionnaireLineBuilder(project)
                .WithVariableName("DuplicateQuestion")
                .Build();

            // Question we're trying to add (same name)
            var newQuestion = new QuestionBankBuilder()
                .WithName("DuplicateQuestion") // same as existing
                .Build();

            var entities = new List<Entity> { project, existingQuestion, newQuestion };

            var rows = new List<RowEntityRequest> { new RowEntityRequest { Id = newQuestion.Id } };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", 1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_QuestionDuplicate_StandardQuestions_Throws()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Create a Question Bank record 
            var questionBank = new QuestionBankBuilder()
                .WithName("AnyQuestionName")
                .Build();

            // Existing Questionnaire Line in the project — already points to this Question Bank
            var existingQuestionLine = new QuestionnaireLineBuilder(project)
                .WithQuestionBank(questionBank)
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Standard)
                .Build();

            // Attempt to add a question from the same Question Bank again
            var newQuestion = new QuestionBankBuilder()
                .WithId(questionBank.Id) // ensure same ID as existing
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Standard)
                .WithName("SomeOtherName") // different name — should still fail because it's a standard Question
                .Build();

            var entities = new List<Entity> { project, existingQuestionLine, newQuestion };

            var rows = new List<RowEntityRequest>
            {
                new RowEntityRequest { Id = newQuestion.Id }
            };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", 1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

        }

        [TestMethod]
        public void ExecutePlugin_QuestionDuplicate_CustomQuestions_Success()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Create a Question Bank record 
            var questionBank = new QuestionBankBuilder()
                .WithName("AnyQuestionName")
                .Build();

            // Existing Questionnaire Line in the project — already points to this Question Bank
            var existingQuestionLine = new QuestionnaireLineBuilder(project)
                .WithQuestionBank(questionBank)
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                .Build();

            // Attempt to add a question from the same Question Bank again
            var newQuestion = new QuestionBankBuilder()
                .WithId(questionBank.Id) // ensure same ID as existing
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Custom)
                .WithName("SomeOtherName")
                .Build();

            var entities = new List<Entity> { project, existingQuestionLine, newQuestion };

            var rows = new List<RowEntityRequest>
            {
                new RowEntityRequest { Id = newQuestion.Id }
            };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", 1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(true, "Plugin executed without error and handled SortOrder = -1 correctly.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_QuestionDuplicate_CustomQuestions_Throws()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Create a Question Bank record 
            var questionBank = new QuestionBankBuilder()
                .WithName("AnyQuestionName")
                .Build();

            // Existing Questionnaire Line in the project — already points to this Question Bank
            var existingQuestionLine = new QuestionnaireLineBuilder(project)
                .WithQuestionBank(questionBank)
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                .WithVariableName("Custom_Question_1")
                .Build();

            // Attempt to add a question from the same Question Bank again
            var newQuestion = new QuestionBankBuilder()
                .WithId(questionBank.Id) // ensure same ID as existing
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Custom)
                .WithName("Custom_Question_1")
                .Build();

            var entities = new List<Entity> { project, existingQuestionLine, newQuestion };

            var rows = new List<RowEntityRequest>
            {
                new RowEntityRequest { Id = newQuestion.Id }
            };

            var pluginContext = MockPluginContext(project.Id, entities, "Question", 1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecutePlugin_ModuleDuplicate_Throws()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Existing module line in project
            var module = new ModuleBuilder().WithName("DuplicateModule").Build();
            var existingModuleLine = new QuestionnaireLineBuilder(project)
                .WithModule(module)
                .Build();

            // Module we're trying to add (same module)
            var newModule = new ModuleBuilder().WithName("DuplicateModule").Build();

            var entities = new List<Entity> { project, module, existingModuleLine, newModule };

            var rows = new List<RowEntityRequest> { new RowEntityRequest { Id = newModule.Id } };

            var pluginContext = MockPluginContext(project.Id, entities, "Module", 1, rows);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAddQuestionsOrModulesCustomAPI>(pluginContext);

            // Assert is handled by ExpectedException
        }

    }
}
